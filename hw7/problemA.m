%blockchain2.m

clear all;

%Build the set Ep of the elliptic curve --------------

p=127;%the prime number we use for the modulo
Ep=[];%the points on the curve will be added in this vector
for i=0:p-1%iteration for 0<=x<=126
    for j=0:p-1%iteration for 0<=y<=126
        y=mod(j^2,p);%left-hand side of the equation of the curve
        x=mod(i^3+7,p);%right-hand side of the equation of the curve
        if y==x%test if both sides are equal
            Ep=[Ep;i,j];%if yes, add the coordinates in the vector of answers
        end
    end
end
N=length(Ep)+1%the number of points of the discontinuous curve plus (infty,infty)

%----------------------------------------------------





%Build the sequence beta_n=n*(19,32)-----------------

n=100;%the size of the sequence
a=[19,32];%the point alpha
b=zeros(n,2);%initializing the sequence
b(1,:)=a;%the first element of the sequence is alpha
for k=2:n
    if b(k-1,1)~= a(1)
        [G,U,V] = gcd(b(k-1,1)-a(1),p);%find Bezout's coefficients to compute division on finite field
        m=mod((b(k-1,2)-a(2))*U,p);
        b(k,1)=mod(m^2-a(1)-b(k-1,1),p);%compute x_n of beta_n
        b(k,2)=mod(m*(2*a(1)+b(k-1,1)-m^2)-a(2),p);%compute y_n of beta_n
    else
        [G,U,V] = gcd(2*a(2),p);%find Bezout's coefficients
        m=mod(3*a(1)^2*U,p);
        b(k,1)=mod(m^2-2*a(1),p);%compute x_n of beta_n
        b(k,2)=mod(m*(3*a(1)-m^2)-a(2),p);%compute y_n of beta_n
    end
end
%plot the sequence and the elements of Ep 
hold on
plot(b(:,1),b(:,2),'x');
plot(Ep(:,1),Ep(:,2),'o')
legend('\beta_n sequence','Elements of E_p')
xlabel('first coordinate')
ylabel('second coordinate')

%-----------------------------------------------



%check for equal points of the sequence --------

sortedseq=sortrows(b);%sort the points of the sequence
verification=0;%will be used to check if there are equal points
for i=2:n
    if sortedseq(i,:)==sortedseq(i-1,:)%check if two neighbor points are equal
        verification=verification + 1;%increase if there is two equal points
    end
end
verification %number of anomalies
    


%-----------------------------------------------


     